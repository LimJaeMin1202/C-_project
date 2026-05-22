using StudyPlanner.Models;

namespace StudyPlanner.Services
{
    // SuperMemo SM-2 간격 반복(Spaced Repetition) 알고리즘
    // - 자가 평가 점수(quality, 0~5)를 받아 다음 복습 간격과 난이도 계수(EF)를 계산한다.
    // - 잘 기억할수록 복습 간격이 길어지고, 못 외울수록 처음부터 다시 시작한다.
    public static class Sm2Service
    {
        // 학습 주제 하나에 복습 결과(quality 0~5)를 반영한다.
        // topic의 RepetitionCount / IntervalDays / EaseFactor / NextReviewDate가 갱신된다.
        public static void ApplyReview(StudyTopic topic, int quality)
        {
            // quality 범위 보정 (0~5)
            if (quality < 0) quality = 0;
            if (quality > 5) quality = 5;

            if (quality >= 3)
            {
                // ── 정답(3점 이상): 복습 성공 → 간격 늘리기 ──
                if (topic.RepetitionCount == 0)
                    topic.IntervalDays = 1;          // 첫 성공: 1일 뒤
                else if (topic.RepetitionCount == 1)
                    topic.IntervalDays = 6;          // 두 번째 성공: 6일 뒤
                else
                    // 그 이후: 직전 간격 × 난이도 계수
                    topic.IntervalDays = (int)Math.Round(topic.IntervalDays * topic.EaseFactor);

                topic.RepetitionCount++;
            }
            else
            {
                // ── 오답(3점 미만): 기억 실패 → 처음부터 다시 ──
                topic.RepetitionCount = 0;
                topic.IntervalDays = 1;
            }

            // ── 난이도 계수(EaseFactor) 갱신 ──
            // 공식: EF' = EF + (0.1 - (5-q) × (0.08 + (5-q) × 0.02))
            //  → 점수가 높을수록 EF가 커지고(간격 빨리 늘어남), 낮을수록 작아진다.
            double q = quality;
            topic.EaseFactor = topic.EaseFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
            if (topic.EaseFactor < 1.3) topic.EaseFactor = 1.3;  // 하한선 1.3 (너무 짧아지지 않게)

            // ── 다음 복습일 = 오늘 + 계산된 간격 ──
            topic.NextReviewDate = DateTime.Today.AddDays(topic.IntervalDays);
        }
    }
}
