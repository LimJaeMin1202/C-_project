using Microsoft.EntityFrameworkCore;
using StudyPlanner.Models;

namespace StudyPlanner.Data
{
    // EF Core 데이터베이스 컨텍스트
    // - StudyTopic 객체와 SQLite 파일(studyplanner.db)을 연결해주는 통로
    public class StudyDbContext : DbContext
    {
        // StudyTopics 테이블: StudyTopic 객체들의 집합
        // (이 프로퍼티가 곧 DB의 테이블 하나가 됨)
        public DbSet<StudyTopic> StudyTopics { get; set; }

        // DB 연결 방법 설정
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // studyplanner.db 라는 로컬 SQLite 파일을 사용
            // (파일이 없으면 EnsureCreated() 호출 시 자동 생성됨)
            optionsBuilder.UseSqlite("Data Source=studyplanner.db");
        }
    }
}
