namespace Peer_Car.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveDocumentAsync(IFormFile file, string folderName);
    }
}
