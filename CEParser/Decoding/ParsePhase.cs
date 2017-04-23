namespace CEParser.Decoding
{
    internal enum ParsePhase
    {
        Looking,
        RecordingQuotedLHS,
        RecordingQuotelessLHS,
        SkippingComments,
        LookingAfterRecordedQuotelessLHS,
        LookingAfterRecordedQuotedLHS,
        LookingForRHS,
        RecordingQuotedRHS,
        RecordingQuotelessRHS,
    }
}