namespace FIAPX.Auth.Model
{
    public class OptionsDto
    {
        public string Region { get; set; } = string.Empty;
        public string UserPoolId { get; set; } = string.Empty;
        public string UserPoolClientId { get; set; } = string.Empty;
        public string ClientPoolSecret { get; set; } = string.Empty;
    }
}
