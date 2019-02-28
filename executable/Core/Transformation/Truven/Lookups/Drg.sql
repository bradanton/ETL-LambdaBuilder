﻿{base},
Standard as (
SELECT distinct cast(SOURCE_CODE as int) SOURCE_CODE, TARGET_CONCEPT_ID, TARGET_DOMAIN_ID, SOURCE_VALID_START_DATE, SOURCE_VALID_END_DATE
FROM Source_to_Standard
WHERE lower(SOURCE_VOCABULARY_ID) IN ('drg')
AND lower(SOURCE_CONCEPT_CLASS_ID) IN ('ms-drg')
AND lower(TARGET_VOCABULARY_ID) IN ('drg')
AND lower(TARGET_CONCEPT_CLASS_ID) IN ('ms-drg')
AND (TARGET_STANDARD_CONCEPT IS NOT NULL or TARGET_STANDARD_CONCEPT != '')
AND (TARGET_INVALID_REASON IS NULL or TARGET_INVALID_REASON = '')
)

select distinct Standard.*
from Standard