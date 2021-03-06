﻿{base},
Standard as (
SELECT distinct SOURCE_CODE, 1 as TARGET_CONCEPT_ID, TARGET_DOMAIN_ID, SOURCE_VALID_START_DATE, SOURCE_VALID_END_DATE
FROM Source_to_Standard
WHERE lower(SOURCE_VOCABULARY_ID) IN ('drg')
AND lower(TARGET_VOCABULARY_ID) IN ('drg')
AND (TARGET_STANDARD_CONCEPT IS NOT NULL or TARGET_STANDARD_CONCEPT != '')
)

select distinct Standard.*
from Standard