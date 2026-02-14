namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for handling file storage operations.
/// Abstracts file storage strategy (local file system, cloud storage, CDN, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="fileContent">File content as byte array.</param>
    /// <param name="fileName">Name of the file to save.</param>
    /// <param name="category">Category/subdirectory for organization (e.g., "branding/logos").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>URL or path to the stored file.</returns>
    Task<string> SaveFileAsync(byte[] fileContent, string fileName, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="filePath">Path or URL of the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deletion successful, false otherwise.</returns>
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the URL for a file.
    /// </summary>
    /// <param name="filePath">Path or identifier of the file.</param>
    /// <returns>Public URL for accessing the file.</returns>
    string GetFileUrl(string filePath);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="filePath">Path or URL of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file information.
    /// </summary>
    /// <param name="filePath">Path or URL of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File size in bytes, or null if not found.</returns>
    Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);
}
