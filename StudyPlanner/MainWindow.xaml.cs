using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using StudyPlanner.Data;
using StudyPlanner.Dialogs;
using StudyPlanner.Models;
using StudyPlanner.Services;
// WinForms 참조로 인한 이름 충돌 방지 (WPF 쪽 명시)
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;

namespace StudyPlanner
{
    /// <summary>
    /// 메인 화면 — 학습 주제 등록/조회 + 오늘의 복습(SM-2 자가 평가)
    /// </summary>
    public partial class MainWindow : Window
    {
        // 자가 평가 대상으로 선택된 학습 주제의 Id
        private int? selectedTopicId = null;

        // 트레이 아이콘 & 알림 서비스
        private TrayNotificationService? trayService;

        // 학습 주제 캐시 (DB에서 한 번 읽고 필터링은 메모리 상에서 수행)
        private List<StudyTopic> allTopics = new();

        // "전체" 옵션 (필터 ComboBox용)
        private const string FilterAllSubjects = "전체";

        public MainWindow()
        {
            InitializeComponent();

            // LiveCharts2 차트 텍스트 한글 깨짐 방지: 맑은 고딕을 전역 폰트로 지정
            LiveCharts.Configure(config =>
                config.HasGlobalSKTypeface(SKTypeface.FromFamilyName("Malgun Gothic")));

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
            LoadDashboard();     // 대시보드 통계/차트 갱신

            // 트레이 아이콘 등록 (앱 실행 중 작업표시줄 트레이에 표시됨)
            trayService = new TrayNotificationService(onOpenRequested: () =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            });

            // 시작 시 오늘 복습할 항목이 있으면 자동 알림
            ShowTodayReviewNotification();

            // 창 닫힐 때 트레이 아이콘 정리
            this.Closed += (s, e) => trayService?.Dispose();
        }

        // 오늘 복습할 항목 수를 트레이 알림으로 표시
        private void ShowTodayReviewNotification()
        {
            using (var db = new StudyDbContext())
            {
                DateTime today = DateTime.Today;
                int dueCount = db.StudyTopics.Count(t => t.NextReviewDate <= today);

                if (dueCount > 0)
                {
                    trayService?.ShowNotification(
                        "오늘의 복습",
                        $"오늘 복습할 항목이 {dueCount}건 있습니다.");
                }
                else
                {
                    trayService?.ShowNotification(
                        "오늘의 복습",
                        "오늘 복습할 항목이 없습니다. 새 주제를 학습해보세요!");
                }
            }
        }

        // [지금 알림 받기] 버튼 클릭 — 즉시 트레이 알림 표시 (시연/테스트용)
        private void btnNotify_Click(object sender, RoutedEventArgs e)
        {
            ShowTodayReviewNotification();
        }

        // DB에서 전체 학습 주제를 캐시에 적재 → 과목 필터 갱신 → 현재 필터 조건 적용
        private void LoadTopics()
        {
            using (var db = new StudyDbContext())
            {
                allTopics = db.StudyTopics
                              .OrderByDescending(t => t.StudyDate)
                              .ToList();
            }
            UpdateSubjectFilterItems();
            ApplyTopicFilter();
        }

        // 과목 필터 ComboBox에 "전체" + 현재 등록된 과목들을 채워넣음
        private void UpdateSubjectFilterItems()
        {
            var current = cmbSubjectFilter.SelectedItem as string;
            var subjects = new List<string> { FilterAllSubjects };
            subjects.AddRange(allTopics.Select(t => t.Subject).Distinct().OrderBy(s => s));
            cmbSubjectFilter.ItemsSource = subjects;
            // 기존 선택 유지 (목록에 없으면 "전체"로)
            cmbSubjectFilter.SelectedItem = (current != null && subjects.Contains(current))
                                            ? current
                                            : FilterAllSubjects;
        }

        // 현재 검색어 + 과목 선택을 캐시에 적용 → DataGrid에 표시
        private void ApplyTopicFilter()
        {
            // 초기화 중 호출 방지
            if (allTopics == null) return;

            string keyword = txtSearch?.Text?.Trim() ?? "";
            string subject = (cmbSubjectFilter?.SelectedItem as string) ?? FilterAllSubjects;

            IEnumerable<StudyTopic> result = allTopics;

            if (subject != FilterAllSubjects && !string.IsNullOrEmpty(subject))
                result = result.Where(t => t.Subject == subject);

            if (!string.IsNullOrEmpty(keyword))
                result = result.Where(t =>
                    (t.Subject?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Unit?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Memo?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));

            var list = result.ToList();
            dgTopics.ItemsSource = list;

            // 필터 결과 개수 표시 (필터가 걸려 있을 때만)
            bool filtered = subject != FilterAllSubjects || !string.IsNullOrEmpty(keyword);
            txtFilterResultCount.Text = filtered
                ? $"({list.Count} / {allTopics.Count})"
                : "";
        }

        // 검색어 입력 변경
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyTopicFilter();

        // 과목 필터 선택 변경
        private void cmbSubjectFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyTopicFilter();

        // 필터 초기화 버튼
        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbSubjectFilter.SelectedItem = FilterAllSubjects;
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
            LoadDashboard();
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
            LoadDashboard();
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
            LoadDashboard();
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
            LoadDashboard();
            MessageBox.Show(
                $"'{exam.Subject}' 시험({exam.DDayText}) 대비 복습 일정을 생성했습니다.\n" +
                $"학습 주제 탭에서 다음 복습일이 시험일 기준으로 재배치된 것을 확인하세요.",
                "일정 생성 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ===================== 대시보드 =====================

        // 요약 카드와 차트(과목별 분포, 망각곡선)를 갱신한다.
        private void LoadDashboard()
        {
            using (var db = new StudyDbContext())
            {
                var topics = db.StudyTopics.ToList();
                var exams = db.Exams.ToList();
                var today = DateTime.Today;

                // ── 요약 카드 ──
                txtTotalTopics.Text = topics.Count.ToString();
                txtTodayReview.Text = topics.Count(t => t.NextReviewDate <= today).ToString();

                var nextExam = exams.Where(e => e.ExamDate >= today)
                                    .OrderBy(e => e.ExamDate)
                                    .FirstOrDefault();
                if (nextExam == null)
                {
                    txtNextExamDay.Text = "—";
                    txtNextExamLabel.Text = "등록된 시험 없음";
                }
                else
                {
                    int dday = (nextExam.ExamDate.Date - today).Days;
                    txtNextExamDay.Text = dday == 0 ? "D-DAY" : $"D-{dday}";
                    txtNextExamLabel.Text = nextExam.Subject;
                }

                // ── 차트 1: 과목별 학습 주제 수 (막대) ──
                var bySubject = topics.GroupBy(t => t.Subject)
                                      .Select(g => new { Subject = g.Key, Count = g.Count() })
                                      .OrderByDescending(x => x.Count)
                                      .ToList();

                chartSubject.LegendPosition = LegendPosition.Hidden;
                chartSubject.TooltipPosition = TooltipPosition.Top;
                chartSubject.Series = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Values = bySubject.Select(x => x.Count).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(63, 81, 181))  // 앱 메인 색
                    }
                };
                chartSubject.XAxes = new[]
                {
                    new Axis
                    {
                        Labels = bySubject.Select(x => x.Subject).ToArray(),
                        LabelsRotation = 0
                    }
                };
                chartSubject.YAxes = new[]
                {
                    new Axis
                    {
                        MinLimit = 0,
                        MinStep = 1,
                        Labeler = v => $"{v:F0}개"
                    }
                };

                // ── 차트 2: 향후 14일 복습 일정 ──
                // 본인 데이터 기반: NextReviewDate가 앞으로 N일 안에 있는 주제를 일별로 카운트
                int scheduleDays = 14;
                int[] reviewsByDay = new int[scheduleDays];
                foreach (var t in topics)
                {
                    int delta = (t.NextReviewDate.Date - today).Days;
                    if (delta >= 0 && delta < scheduleDays)
                        reviewsByDay[delta]++;
                }

                string[] dayLabels = Enumerable.Range(0, scheduleDays)
                    .Select(i =>
                    {
                        if (i == 0) return "오늘";
                        if (i == 1) return "내일";
                        return today.AddDays(i).ToString("M/d");
                    })
                    .ToArray();

                chartSchedule.LegendPosition = LegendPosition.Hidden;
                chartSchedule.TooltipPosition = TooltipPosition.Top;

                chartSchedule.Series = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Values = reviewsByDay,
                        Fill = new SolidColorPaint(new SKColor(76, 175, 80))  // 초록 (복습 = 학습 활동)
                    }
                };
                chartSchedule.XAxes = new[]
                {
                    new Axis
                    {
                        Labels = dayLabels,
                        LabelsRotation = 0
                    }
                };
                chartSchedule.YAxes = new[]
                {
                    new Axis
                    {
                        MinLimit = 0,
                        MinStep = 1,
                        Labeler = v => $"{v:F0}개"   // 'N개' 형태로 표시 (가로로 자연스럽게 읽힘)
                    }
                };
            }
        }

        // ===================== 학습 주제 삭제/편집 =====================

        // 학습 주제 행의 [휴지통] 버튼 클릭 → 확인 후 DB에서 삭제
        private void btnDeleteTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudyTopic topic)
            {
                var result = MessageBox.Show(
                    $"'{topic.Subject} - {topic.Unit}' 주제를 정말 삭제하시겠습니까?",
                    "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                using (var db = new StudyDbContext())
                {
                    var tracked = db.StudyTopics.Find(topic.Id);
                    if (tracked != null)
                    {
                        db.StudyTopics.Remove(tracked);
                        db.SaveChanges();
                    }
                }

                LoadTopics();
                LoadReviewList();
                LoadDashboard();
            }
        }

        // 학습 주제 행 더블클릭 → 편집 다이얼로그 열기
        private void dgTopics_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 헤더나 빈 공간 더블클릭은 무시
            if (dgTopics.SelectedItem is not StudyTopic selected) return;

            using (var db = new StudyDbContext())
            {
                var topic = db.StudyTopics.Find(selected.Id);
                if (topic == null) return;

                var dlg = new EditTopicDialog(topic) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    db.SaveChanges();   // 다이얼로그가 topic 객체를 직접 수정함
                    LoadTopics();
                    LoadReviewList();
                    LoadDashboard();
                }
            }
        }

        // ===================== 시험 삭제/편집 =====================

        private void btnDeleteExam_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Exam exam)
            {
                var result = MessageBox.Show(
                    $"'{exam.Subject} - {exam.Name}' 시험을 정말 삭제하시겠습니까?",
                    "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                using (var db = new StudyDbContext())
                {
                    var tracked = db.Exams.Find(exam.Id);
                    if (tracked != null)
                    {
                        db.Exams.Remove(tracked);
                        db.SaveChanges();
                    }
                }

                LoadExams();
                LoadDashboard();
            }
        }

        private void dgExams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgExams.SelectedItem is not Exam selected) return;

            using (var db = new StudyDbContext())
            {
                var exam = db.Exams.Find(selected.Id);
                if (exam == null) return;

                var dlg = new EditExamDialog(exam) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    db.SaveChanges();
                    LoadExams();
                    LoadDashboard();
                }
            }
        }
    }
}
