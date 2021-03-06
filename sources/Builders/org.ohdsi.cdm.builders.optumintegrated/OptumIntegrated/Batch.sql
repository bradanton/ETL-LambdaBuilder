﻿with eligDates AS 
(
SELECT distinct
ptid,
CASE
WHEN eligeff < '05/01/2000' THEN '05/01/2000'	
ELSE eligeff 
END elig_start_date,	
CASE 
WHEN eligend < '05/01/2000' THEN '05/01/2000'
ELSE eligend
END elig_end_date
FROM {sc}.member_detail as md
),
clinDates AS 
(
select distinct
p.ptid,
CAST(SUBSTRING(first_month_active, 5, 2) + '/01/' + SUBSTRING(first_month_active, 1, 4) as DATE) clin_start_date,
first_month_active,
DATEADD(DAY, -1, DATEADD(MONTH, 1, CAST(SUBSTRING(last_month_active, 5, 2) + '/01/' + SUBSTRING(last_month_active, 1, 4) AS DATE))) clin_end_date,
last_month_active
FROM {sc}.patient as p
where lower(birth_yr) != 'unknown'
)

SELECT DISTINCT {0} cast(replace(lower(p.ptid), 'pt','') as bigint) AS person_id, p.ptid
from eligDates as md
LEFT JOIN clinDates p ON md.ptid = p.ptid
where elig_start_date < clin_end_date
AND clin_start_date < elig_end_date
order by cast(replace(lower(p.ptid), 'pt','') as bigint)