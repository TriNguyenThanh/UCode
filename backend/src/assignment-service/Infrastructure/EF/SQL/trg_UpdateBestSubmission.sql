CREATE OR ALTER TRIGGER trg_UpdateBestSubmission
ON Submissions
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Chỉ xử lý submission có ít nhất 1 test pass
    IF NOT EXISTS (SELECT 1 FROM inserted WHERE PassedTestcase > 0)
        RETURN;
    
    -- Xử lý từng submission mới
    ;WITH NewSubmissions AS (
        SELECT 
            i.SubmissionId,
            i.AssignmentUserId,
            i.ProblemId,
            i.PassedTestcase,
            i.TotalTestcase,
            i.TotalTime,
            i.TotalMemory,
            i.SubmittedAt
        FROM inserted i
        WHERE i.PassedTestcase > 0
    ),
    CurrentBest AS (
        SELECT 
            ns.SubmissionId AS NewSubmissionId,
            ns.AssignmentUserId,
            ns.ProblemId,
            ns.PassedTestcase AS NewScore,
            ns.TotalTestcase AS NewMaxScore,
            ns.TotalTime AS NewTime,
            ns.TotalMemory AS NewMemory,
            ns.SubmittedAt AS NewSubmittedAt,
            bs.BestSubmissionId,
            bs.SubmissionId AS CurrentSubmissionId,
            bs.Score AS CurrentScore,
            bs.TotalTime AS CurrentTime,
            bs.TotalMemory AS CurrentMemory,
            bs.UpdatedAt AS CurrentUpdatedAt
        FROM NewSubmissions ns
        LEFT JOIN BestSubmissions bs 
            ON ns.AssignmentUserId = bs.AssignmentUserId 
            AND ns.ProblemId = bs.ProblemId
    ),
    ShouldUpdate AS (
        SELECT 
            NewSubmissionId,
            AssignmentUserId,
            ProblemId,
            NewScore,
            NewMaxScore,
            NewTime,
            NewMemory,
            BestSubmissionId,
            CASE
                -- Case 1: Chưa có BestSubmission (submission đầu tiên)
                WHEN BestSubmissionId IS NULL THEN 1
                
                -- Case 2: Điểm cao hơn
                WHEN NewScore > CurrentScore THEN 1
                
                -- Case 3: Điểm bằng nhau, nhưng time nhanh hơn
                WHEN NewScore = CurrentScore AND NewTime < CurrentTime THEN 1
                
                -- Case 4: Điểm và time bằng nhau, nhưng memory ít hơn
                WHEN NewScore = CurrentScore 
                     AND NewTime = CurrentTime 
                     AND NewMemory < CurrentMemory THEN 1
                
                -- Case 5: Tất cả bằng nhau, ưu tiên submission nộp sớm hơn
                WHEN NewScore = CurrentScore 
                     AND NewTime = CurrentTime 
                     AND NewMemory = CurrentMemory 
                     AND NewSubmittedAt < CurrentUpdatedAt THEN 1
                
                -- Ngược lại: không update
                ELSE 0
            END AS ShouldUpdate
        FROM CurrentBest
    )
    -- MERGE: Xử lý cả INSERT (chưa có) và UPDATE (có rồi nhưng submission mới tốt hơn)
    MERGE BestSubmissions AS target
    USING (
        SELECT 
            NewSubmissionId,
            AssignmentUserId,
            ProblemId,
            NewScore,
            NewMaxScore,
            NewTime,
            NewMemory
        FROM ShouldUpdate
        WHERE ShouldUpdate = 1
    ) AS source
    ON target.AssignmentUserId = source.AssignmentUserId 
       AND target.ProblemId = source.ProblemId
    
    -- Trường hợp 1: Đã có BestSubmission → UPDATE
    WHEN MATCHED THEN
        UPDATE SET 
            SubmissionId = source.NewSubmissionId,
            Score = source.NewScore,
            MaxScore = source.NewMaxScore,
            TotalTime = source.NewTime,
            TotalMemory = source.NewMemory,
            UpdatedAt = GETUTCDATE()
    
    -- Trường hợp 2: Chưa có BestSubmission → INSERT (submission đầu tiên)
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (
            BestSubmissionId,
            AssignmentUserId,
            ProblemId,
            SubmissionId,
            Score,
            MaxScore,
            TotalTime,
            TotalMemory,
            UpdatedAt
        )
        VALUES (
            NEWID(),
            source.AssignmentUserId,
            source.ProblemId,
            source.NewSubmissionId,
            source.NewScore,
            source.NewMaxScore,
            source.NewTime,
            source.NewMemory,
            GETUTCDATE()
        );
END;
GO