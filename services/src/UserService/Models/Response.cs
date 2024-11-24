namespace UserService.Models;

public class Response
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; }
}