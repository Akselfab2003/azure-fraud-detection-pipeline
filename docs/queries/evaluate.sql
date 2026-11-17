-- docs/queries/evaluate.sql
-- Baseline: how well do the rules catch the simulator's labelled fraud?
-- Only transactions that Scoring has stored are counted.
WITH scored as (
SELECT 
    g.pattern,
    g.is_fraud,
    (c.id is not null) as flagged
  from  ground_truth g
  join transactions t on g.transaction_id = t.id
  left join cases c on t.id = c.transaction_id
)

SELECT COALESCE(pattern, 'ALL') AS pattern,
       COUNT(*) FILTER (WHERE is_fraud)                      AS fraud,
       COUNT(*) FILTER (WHERE is_fraud AND flagged)          AS caught,
       ROUND(100.0 * COUNT(*) FILTER (WHERE is_fraud AND flagged)
             / NULLIF(COUNT(*) FILTER (WHERE is_fraud), 0), 1)   AS recall_pct,
       COUNT(*) FILTER (WHERE flagged)                       AS cases,
       ROUND(100.0 * COUNT(*) FILTER (WHERE is_fraud AND flagged)
             / NULLIF(COUNT(*) FILTER (WHERE flagged), 0), 1)    AS precision_pct,
       COUNT(*) FILTER (WHERE NOT is_fraud AND flagged)      AS false_positives
FROM scored
GROUP BY ROLLUP (pattern)
ORDER BY pattern NULLS LAST;
