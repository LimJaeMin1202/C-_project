# 망각곡선 학습 플래너 (Study Planner)

> **Forgetting Curve-based Study Planner for Korean University Students**
> 잊어버리기 전에 알려주는 똑똑한 학습 도우미

한국 대학생을 위한 **망각곡선 기반 학습 플래너** 데스크탑 애플리케이션입니다.
C# 프로그래밍 강의 기말 대체 과제로 제작되었습니다.

---

## 📌 프로젝트 개요

대학생은 한 학기에 평균 4~6과목을 동시에 수강하며, 중간고사·기말고사·과제를 병행합니다.
기존 학습 관리 도구(노션, 구글 캘린더, 에브리타임 등)는 **단순 기록**에 그쳐,
"이 내용을 **언제 다시 복습해야 가장 효율적인지**"에 대한 과학적 근거를 제시하지 못합니다.

본 프로젝트는 1885년 헤르만 에빙하우스(Hermann Ebbinghaus)의 **망각곡선(Forgetting Curve)** 이론과
**SuperMemo SM-2 알고리즘**을 적용하여, 사용자의 자가 평가에 따라 최적의 복습 시점을 자동으로 계산합니다.

---

## 🆚 기존 도구와의 차별점

| 구분 | SuperMemo / Anki | 노션 / 구글캘린더 | 에브리타임 | **본 프로젝트** |
|------|:---:|:---:|:---:|:---:|
| 망각곡선 자동 적용 | ✅ | ❌ | ❌ | ✅ |
| 한국어 인터페이스 | △ | ✅ | ✅ | ✅ |
| 대학 시험(중간/기말) 특화 | ❌ | ❌ | △ | ✅ |
| 시험 D-Day 역산 | ❌ | ❌ | ❌ | ✅ |
| 오프라인 동작 | △ | ❌ | ❌ | ✅ |
| 자가 평가 기반 추천 | ✅ | ❌ | ❌ | ✅ |

- **SuperMemo / Anki**: 영어 단어 암기용 플래시카드 → 한국 대학 시험(서술형·풀이형)에 부적합
- **노션 / 구글캘린더**: 모든 복습 시점을 사용자가 수동 입력해야 함
- **본 프로젝트**: 망각곡선 자동 적용 + 한국 대학 학사 일정 특화 + 100% 오프라인(개인정보 외부 전송 없음)

---

## ✨ 주요 기능

1. **학습 주제 등록** — 과목 / 단원 / 학습일 / 메모 입력
2. **SM-2 자동 복습 알고리즘** — 학습 직후 자가 평가(0~5점) → 다음 복습일 자동 계산 (1일 → 3일 → 7일 → 14일 → 30일)
3. **시험 D-Day 역산** — 시험일 입력 시, 시험 직전까지 충분히 복습되도록 일정 자동 재조정
4. **망각곡선 시각화** — 주제별 현재 기억 보존율 그래프
5. **통계 대시보드** — 과목별 학습 시간, 약점 단원 자동 식별, 학습 패턴 분석
6. **복습 알림** — 오늘 복습할 항목 일일 요약 (Windows 트레이 알림)

---

## 🛠️ 기술 스택

| 항목 | 사용 기술 |
|------|-----------|
| 언어 | C# |
| 프레임워크 | .NET 8.0 (LTS) |
| UI | WPF (XAML) |
| 데이터베이스 | SQLite (Entity Framework Core 8.0.11) |
| 차트 | LiveCharts2 (LiveChartsCore.SkiaSharpView.WPF) |
| UI 테마 | MaterialDesignThemes |
| 개발 환경 | Visual Studio 2022 Community |
| 핵심 알고리즘 | SuperMemo SM-2 |

---

## 🚀 빌드 및 실행

### 요구 사항
- Windows 10/11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (".NET 데스크톱 개발" 워크로드)

### 실행 방법
```bash
# 저장소 클론
git clone <저장소 주소>
cd StudyPlanner

# 패키지 복원 및 실행
dotnet restore
dotnet run --project StudyPlanner
```
또는 `StudyPlanner.sln`을 Visual Studio 2022에서 열고 `Ctrl + F5`로 실행합니다.

---

## 📂 프로젝트 구조

```
StudyPlanner/
├── StudyPlanner.sln
├── StudyPlanner/
│   ├── Models/        # 데이터 모델 (StudyTopic 등)
│   ├── Services/      # SM-2 알고리즘, DB, 통계 로직
│   ├── Views/         # 화면 (XAML)
│   ├── App.xaml
│   └── MainWindow.xaml
└── README.md
```
> 폴더 구조는 개발 진행에 따라 변경될 수 있습니다.

---

## 📅 개발 일정 (3주)

| 주차 | 작업 |
|------|------|
| 1주차 | WPF UI 설계, SQLite DB 설계, 학습 주제 등록 모듈 |
| 2주차 | SM-2 알고리즘 구현, 자가 평가 시스템, 시험 D-Day 역산 |
| 3주차 | 차트 시각화, 알림 시스템, UI 다듬기, 보고서 작성 |

---

## 📚 참고 문헌

- Ebbinghaus, H. (1885). *Über das Gedächtnis: Untersuchungen zur experimentalen Psychologie.*
- Cepeda, N. J., et al. (2006). Distributed practice in verbal recall tasks: A review and quantitative synthesis. *Psychological Bulletin, 132(3).*
- Dunlosky, J., et al. (2013). Improving Students' Learning With Effective Learning Techniques. *Psychological Science in the Public Interest, 14(1).*
- Wozniak, P. A. (1990). *Optimization of learning.* (SuperMemo SM-2 알고리즘)
- Murre, J. M. J., & Dros, J. (2015). Replication and Analysis of Ebbinghaus' Forgetting Curve. *PLoS ONE, 10(7).*

---

*본 프로젝트는 학습/교육 목적으로 제작되었습니다.*
