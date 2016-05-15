namespace UtmCliUtility
{
    public struct ProcessingResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string StackTrace { get; set; }
        public string ResultTextData { get; set; }
    }
}
