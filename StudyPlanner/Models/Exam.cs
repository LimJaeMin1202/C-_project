using System.ComponentModel.DataAnnotations.Schema;

namespace StudyPlanner.Models
{
    // 시험 정보 (DB 테이블 한 행에 대응)
    public class Exam
    {
        public int Id { get; set; }                              // 기본키
        public string Subject { get; set; } = "";                // 과목명 (학습 주제의 과목과 연결됨)
        public string Name { get; set; } = "";                   // 시험명 (예: 중간고사)
        public DateTime ExamDate { get; set; } = DateTime.Today; // 시험 날짜

        // ===== 아래 2개는 계산용 프로퍼티 (DB에 저장 안 함 = NotMapped) =====

        // D-Day 숫자: 시험일까지 남은 일수 (양수=남음, 0=당일, 음수=지남)
        [NotMapped]
        public int DDay => (ExamDate.Date - DateTime.Today).Days;

        // 화면 표시용 텍스트: D-7 / D-DAY / D+3
        [NotMapped]
        public string DDayText => DDay > 0 ? $"D-{DDay}"
                                : DDay == 0 ? "D-DAY"
                                : $"D+{-DDay}";
    }
}
