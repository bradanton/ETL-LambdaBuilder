createVisitOccurrenceTests <- function() {
  
  patient <- createPatient()
  claim <- createClaim()
  declareTest("Creates multiple IP admission claims with different pat_planids, should get a single IP visit occurrence.", 
              id = patient$person_id)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01', rvnu_cd = '0100', pos = '21',
                     pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01', rvnu_cd = '0100', pos = '21',
                     pat_planid = patient$patid*1000, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  expect_count_visit_occurrence(rowCount = 1, person_id = patient$person_id, visit_concept_id = 9201)
  expect_count_visit_detail(rowCount = 1, person_id = patient$person_id, visit_detail_concept_id = 9201)
  
  
  patient <- createPatient()
  claim <- createClaim()
  declareTest("Creates an Inpatient Visit Occurrence for a patient with a single medical diagnosis.", 
              id = patient$person_id)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01', rvnu_cd = '0100', pos = '21',
                     pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  expect_visit_occurrence(person_id = patient$person_id, visit_concept_id = 9201)
  expect_visit_detail(person_id = patient$person_id, visit_detail_concept_id = 9201)

  
  patient <- createPatient()
  claim <- createClaim()
  declareTest("Creates an Outpatient Visit Occurrence for a patient with a single medical diagnosis.", 
              id = patient$person_id)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01',
                     pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678', pos = '22')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  expect_visit_occurrence(person_id = patient$person_id, visit_concept_id = 9202)
  expect_visit_detail(person_id = patient$person_id, visit_detail_concept_id = 9202)


  patient <- createPatient()
  claim <- createClaim()
  declareTest("Creates an Emergency Room Visit Occurrence for a patient with a single medical diagnosis.", 
              id = patient$person_id)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01', rvnu_cd = '0981', pos = '23',
                     pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  expect_visit_occurrence(person_id = patient$person_id, visit_concept_id = 9203)
  expect_visit_detail(person_id = patient$person_id, visit_detail_concept_id = 8870)
  

  patient <- createPatient()
  claim <- createClaim()
  declareTest("Creates an Long Term Care Visit Occurrence for a patient with a single medical diagnosis.", 
              id = patient$person_id)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01', rvnu_cd = '0100', pos = '13',
                     pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  expect_visit_occurrence(person_id = patient$person_id, visit_concept_id = 42898160)
  expect_visit_detail(person_id = patient$person_id, visit_detail_concept_id = 8615)


  patient <- createPatient()
  claim <- createClaim()
  declareTest("Patient has a series of diagnoses that fall outside of their Observation Period, but record stays the same.", 
              id = patient$person_id)
  add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
  add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2009-07-01', pos = '22',
                     pat_planid = patient$patid, patid = patient$patid, fst_dt = '2009-07-01', prov = '111111', provcat = '5678')
  add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
  expect_visit_occurrence(person_id = patient$person_id, visit_concept_id = 9202, visit_start_date = '2009-07-01')
 
  
#   discStats <- read.csv(file = "inst/csv/discharge_status.csv", header = T, as.is = T)
#   for (i in 1:nrow(discStats)) {
#     patient <- createPatient()
#     claim <- createClaim()
#     declareTest(sprintf("Test discharge status mapping %s", discStats[i,]$SOURCE_CODE),
#                 id = patient$person_id)
#     add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
#                       gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
#     add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01', dstatus = discStats[i,]$SOURCE_CODE,
#                          pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', prov = '111111', provcat = '5678')
#     add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
#     expect_visit_occurrence(person_id = patient$person_id, 
#                             discharge_to_concept_id = discStats[i,]$TARGET_CONCEPT_ID,
#                             discharge_to_source_value = discStats[i,]$SOURCE_CODE)
#     # expect_visit_detail(person_id = patient$person_id, 
#     #                         discharge_to_concept_id = discStats[i,]$TARGET_CONCEPT_ID,
#     #                         discharge_to_source_value = discStats[i,]$SOURCE_CODE)
#   }
# 
#   patient <- createPatient()
#   claim <- createClaim()
#   declareTest(sprintf("Patient has a series of visits that are linked by foreign key (%d)", i), 
#               id = patient$person_id)
#   add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
#                     gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
#   
#   add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-07-01',
#                      pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-07-01', 
#                      prov = '111111', provcat = '5678')
#   add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9",
#                     diag = "7061", diag_position = 1, clmid = claim$clmid)
#     
#   firstOccurrenceId <- lookup_visit_occurrence(fetchField = "visit_occurrence_id", 
#                                                person_id = patient$person_id, visit_start_date = '2013-07-01')
#   
#   # firstDetailId <- lookup_visit_occurrence(fetchField = "visit_detail_id", 
#   #                                              person_id = patient$person_id, visit_detail_start_date = '2013-07-01')
#   
#   claim <- createClaim()
#   add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-10-01',
#                      pat_planid = patient$patid, patid = patient$patid, 
#                      fst_dt = '2013-10-01', prov = '111111', provcat = '5678')
#   add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", 
#                     diag = "7061", diag_position = 1, clmid = claim$clmid)
#   
#   expect_visit_occurrence(person_id = patient$person_id,
#                           preceding_visit_occurrence_id = firstOccurrenceId)
#   # expect_visit_detail(person_id = patient$person_id,
#   #                     preceding_visit_detail_id = firstDetailId)
#   
#   # Adding inpatient confinment 
#   # TODO cleanup
#   
#   patient <- createPatient()
#   claim <- createClaim()
#   declareTest("Patient has a series of inpatient_confinement records", 
#               id = patient$person_id)
#   add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2015-10-31',
#                                    gdr_cd = 'F', patid = patient$patid, yrdob = 1959)
#   
#   add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2013-08-21',
#                      pat_planid = patient$patid, patid = patient$patid, fst_dt = '2013-08-21', 
#                      prov = '111111', provcat = '5678', pos = '20', conf_id = '123')
#   add_inpatient_confinement(patid = patient$patid, pat_planid = patient$patid, admit_date = '2013-08-11', 
#                             diag1 = '250.00', disch_date = '2013-08-22', icd_flag = '9', conf_id = '123')
#   add_inpatient_confinement(patid = patient$patid, pat_planid = patient$patid, admit_date = '2015-02-11', 
#                             diag1 = '250.00', disch_date = '2015-02-15', icd_flag = '9', conf_id = '123')
#   
#   
#   patient <- createPatient()
#   claim <- createClaim()
#   declareTest("Creates an Emergency Room Visit Occurrence in the future, record should be deleted.", 
#               id = patient$person_id)
#   add_member_continuous_enrollment(eligeff = '2010-05-01', eligend = '2013-10-31',
#                                    gdr_cd = 'F', patid = patient$patid, yrdob = 1969)
#   add_medical_claims(clmid = claim$clmid, clmseq = '001', lst_dt = '2099-07-01', rvnu_cd = '0981', pos = '23',
#                      pat_planid = patient$patid, patid = patient$patid, fst_dt = '2099-07-01', prov = '111111', provcat = '5678')
#   add_med_diagnosis(patid = patient$patid, pat_planid = patient$patid, icd_flag = "9", diag = "7061", diag_position = 1, clmid = claim$clmid)
#   expect_no_visit_occurrence(person_id = patient$person_id, visit_concept_id = 9203)
#   expect_no_visit_detail(person_id = patient$person_id, visit_detail_concept_id = 9203)
 }
