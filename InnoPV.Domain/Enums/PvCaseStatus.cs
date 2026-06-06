namespace InnoPV.Domain.Enums;

public enum PvCaseStatus
{
    Draft = 1,
    DataEntryInProgress = 2,
    ValidityPending = 3,
    InvalidFollowUpRequired = 4,
    DuplicateCheckPending = 5,

    PvAssociateChecklistPending = 6,
    SubmittedToPvManager = 7,

    PvManagerReviewPending = 8,
    PvManagerChecklistPending = 9,
    ReturnedByPvManager = 10,
    ResubmittedToPvManager = 11,

    ForwardedToMedicalReviewer = 12,
    MedicalReviewPending = 13,
    MedicalReviewerChecklistPending = 14,
    ReturnedByMedicalReviewer = 15,

    AdditionalInformationRequired = 16,
    MedicallyApproved = 17,

    CaseFinalized = 18,
    SubmissionPending = 19,
    Submitted = 20,
    AcknowledgementPending = 21,
    CaseClosed = 22,

    Reopened = 23,
    MarkedAsDuplicate = 24,
    MarkedAsInvalid = 25,
    OnHold = 26,



    ReturnedToPvAssociate = 27,

    PvManagerReviewInProgress = 28,

    SubmittedToMedicalReviewer = 29,

    ReturnedToPvManager = 30,

    MedicalReviewInProgress = 31,

}