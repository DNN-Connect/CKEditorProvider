using Newtonsoft.Json.Linq;


namespace ElFinder.DTO
{
    internal static class Error
    {       
        public static JObject CommandNotFound()
        {
            return FormatSimpleError("errUnknownCmd");
        }
        public static JObject MissedParameter(string command)
        {
            return Json(new { error = new string[] { "errCmdParams", command } });
        }
        public static JObject CannotUploadFile()
        {
            return FormatSimpleError("errUploadFile");
        }
        public static JObject MaxUploadFileSize()
        {
            return FormatSimpleError("errFileMaxSize");
        }
        public static JObject AccessDenied()
        {
            return FormatSimpleError("errAccess");
        }
        private static JObject FormatSimpleError(string message)
        {
            return Json(new { error = message });
        }
        private static JObject Json(object data)
        {
            return JObject.FromObject(data);
        }
    }
}