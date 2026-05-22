using System.Windows;
using System.Windows.Controls;
using StudyPlanner.Data;
using StudyPlanner.Models;
using StudyPlanner.Services;

namespace StudyPlanner
{
    /// <summary>
    /// 메인 화면 — 학습 주제 등록/조회 + 오늘의 복습(SM-2 자가 평가)
    /// </summary>
    public partial class MainWindow : Window
    {
        // 자가 평가 대상으로 선택된 학습 주제의 Id
        private int? selectedTopicId = null;

        public MainWindow()
        {
            InitializeComponent();

            // 프로그램 시작 시: DB 파일이 없으면 자동 생성
            using (var db = new StudyDbContext())
            {
                db.Database.EnsureCreated();
            }

            dpStudyDate.SelectedDate = DateTime.Today;          // 학습일 기본값 = 오늘
            dpExamDate.SelectedDate = DateTime.Today.AddDays(14); // 시험일 기본값 = 2주 뒤
            LoadTopics();        // 학습 주제 목록 불러오기
            LoadReviewList();    // 오늘 복습할 항목 불러오기
            LoadExams();         // 시험 목록 불러오기
        }

        // DB에서 전체 학습 주제 목록을 불러와 표시 (학습 주제 탭)
        private void LoadTopics()
        {
            using (var db = new StudyDbContext())
            {
                dgTopics.ItemsSource = db.StudyTopics
                                         .OrderByDescending(t => t.StudyDate)
                                         .ToList();
            }
        }

        // 오늘(또는 그 이전)이 복습 예정일인 항목만 불러와 표시 (오늘의 복습 탭)
        private void LoadReviewList()
        {
            using (var db = new StudyDbContext())
            {
                DateTime today = DateTime.Today;
                var dueList = db.StudyTopics
                                .Where(t => t.NextReviewDate <= today)
                                .OrderBy(t => t.NextReviewDate)
                                .ToList();

                dgReview.ItemsSource = dueList;
                txtReviewCount.Text = $"오늘 복습할 항목 ({dueList.Count}개)";
            }
        }

        // [등록] 버튼 클릭 — 새 학습 주제 저장
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // --- 입력 검증 ---
            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                MessageBox.Show("과목을 입력해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("단원/주제를 입력해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime studyDate = dpStudyDate.SelectedDate ?? DateTime.Today;

            // --- 새 학습 주제 객체 생성 ---
            var topic = new StudyTopic
            {
                Subject = txtSubject.Text.Trim(),
                Unit = txtUnit.Text.Trim(),
                StudyDate = studyDate,
                Memo = txtMemo.Text.Trim(),
                NextReviewDate = studyDate   // 학습한 날 바로 첫 복습 대상 (이후 SM-2가 간격 계산)
            };

            using (var db = new StudyDbContext())
            {
                db.StudyTopics.Add(topic);
                db.SaveChanges();
            }

            // --- 입력 폼 초기화 + 목록 새로고침 ---
            txtSubject.Clear();
            txtUnit.Clear();
            txtMemo.Clear();
            dpStudyDate.SelectedDate = DateTime.Today;
            LoadTopics();
            LoadReviewList();
        }

        // 오늘의 복습 목록에서 항목을 선택했을 때
        private void dgReview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgReview.SelectedItem is StudyTopic t)
            {
                selectedTopicId = t.Id;
                txtSelected.Text = $"[{t.Subject}] {t.Unit}\n" +
                                   $"현재 반복 {t.RepetitionCount}회 · 난이도(EF) {t.EaseFactor:F2}";
            }
            else
            {
                selectedTopicId = null;
                txtSelected.Text = "복습할 항목을 선택하세요.";
            }
        }

        // 자가 평가 점수 버튼(0~5) 클릭 — SM-2 알고리즘 적용
        private void btnRate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTopicId == null)
            {
                MessageBox.Show("먼저 복습할 항목을 선택하세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 버튼의 Tag에 담긴 점수(0~5) 읽기
            int quality = int.Parse(((Button)sender).Tag.ToString()!);

            using (var db = new StudyDbContext())
            {
                // 선택된 주제를 DB에서 다시 찾아옴 (Id로 조회)
                var topic = db.StudyTopics.Find(selectedTopicId.Value);
                if (topic == null) return;

                // ★ SM-2 알고리즘 적용 → 다음 복습일/간격/EF 자동 계산
                Sm2Service.ApplyReview(topic, quality);
                db.SaveChanges();

                MessageBox.Show(
                    $"복습 완료!\n\n다음 복습일: {topic.NextReviewDate:yyyy-MM-dd}\n" +
                    $"복습 간격: {topic.IntervalDays}일\n난이도(EF): {topic.EaseFactor:F2}",
                    "SM-2 결과", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // 선택 해제 + 양쪽 목록 새로고침
            selectedTopicId = null;
            txtSelected.Text = "복습할 항목을 선택하세요.";
            LoadTopics();
            LoadReviewList();
        }

        // ===================== 시험 D-Day =====================

        // DB에서 시험 목록을 불러와 표시 (시험일 가까운 순)
        private void LoadExams()
        {
            using (var db = new StudyDbContext())
            {
                dgExams.ItemsSource = db.Exams
                                        .OrderBy(x => x.ExamDate)
                                        .ToList();
            }
        }

        // [시험 등록] 버튼 클릭
        private void btnAddExam_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExamSubject.Text))
            {
                MessageBox.Show("과목을 입력해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dpExamDate.SelectedDate == null)
            {
                MessageBox.Show("시험일을 선택해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var exam = new Exam
            {
                Subject = txtExamSubject.Text.Trim(),
                Name = string.IsNullOrWhiteSpace(txtExamName.Text) ? "시험" : txtExamName.Text.Trim(),
                ExamDate = dpExamDate.SelectedDate.Value
            };

            using (var db = new StudyDbContext())
            {
                db.Exams.Add(exam);
                db.SaveChanges();
            }

            txtExamSubject.Clear();
            txtExamName.Clear();
            dpExamDate.SelectedDate = DateTime.Today.AddDays(14);
            LoadExams();
        }

        // [시험 대비 복습 일정 생성] 버튼 클릭 — D-Day 역산
        // 선택한 시험의 과목에 속한 학습 주제들을, 시험일에서 거꾸로 하루씩 앞당겨 복습 예약한다.
        private void btnGenPlan_Click(object sender, RoutedEventArgs e)
        {
            if (dgExams.SelectedItem is not Exam exam)
            {
                MessageBox.Show("먼저 목록에서 시험을 선택하세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new StudyDbContext())
            {
                // 해당 과목의 학습 주제들 가져오기
                var topics = db.StudyTopics
                               .Where(t => t.Subject == exam.Subject)
                               .ToList();

                if (topics.Count == 0)
                {
                    MessageBox.Show($"'{exam.Subject}' 과목의 학습 주제가 없습니다.\n먼저 학습 주제를 등록하세요.",
                                    "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int daysUntil = (exam.ExamDate.Date - DateTime.Today).Days;

                if (daysUntil <= 1)
                {
                    // 시험이 오늘/내일/지났으면 → 전부 지금 복습
                    foreach (var t in topics)
                        t.NextReviewDate = DateTime.Today;
                }
                else
                {
                    // 시험일에서 역산: 마지막 주제는 시험 하루 전, 그 앞은 이틀 전... 순으로 분산
                    // (남은 기간을 넘어가면 순환하여 같은 구간에 배치)
                    int window = daysUntil - 1;  // 오늘 다음날 ~ 시험 전날
                    for (int i = 0; i < topics.Count; i++)
                    {
                        int daysBeforeExam = (i % window) + 1;            // 1 ~ window
                        topics[i].NextReviewDate = exam.ExamDate.Date.AddDays(-daysBeforeExam);
                    }
                }

                db.SaveChanges();
            }

            LoadTopics();
            LoadReviewList();
            MessageBox.Show(
                $"'{exam.Subject}' 시험({exam.DDayText}) 대비 복습 일정을 생성했습니다.\n" +
                $"학습 주제 탭에서 다음 복습일이 시험일 기준으로 재배치된 것을 확인하세요.",
                "일정 생성 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
