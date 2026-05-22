namespace StudyPlanner.Models
{
    // 학습 주제 한 건을 표현하는 데이터 모델 (DB의 한 행에 대응)
    public class StudyTopic
    {
        public int Id { get; set; }                               // 기본키 (자동 증가)
        public string Subject { get; set; } = "";                 // 과목명 (예: 자료구조)
        public string Unit { get; set; } = "";                    // 단원 / 주제 (예: 이진 탐색 트리)
        public DateTime StudyDate { get; set; } = DateTime.Today; // 학습한 날짜
        public string Memo { get; set; } = "";                    // 메모

        // ===== 아래 4개는 2주차 SM-2 알고리즘에서 사용 (지금은 기본값만) =====
        public int RepetitionCount { get; set; } = 0;             // 복습 성공 횟수 (n)
        public double EaseFactor { get; set; } = 2.5;             // 난이도 계수 (EF), 기본 2.5
        public int IntervalDays { get; set; } = 0;                // 현재 복습 간격(일)
        public DateTime NextReviewDate { get; set; } = DateTime.Today; // 다음 복습 예정일
    }
}
