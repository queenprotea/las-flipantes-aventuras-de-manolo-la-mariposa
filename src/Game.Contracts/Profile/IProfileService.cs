using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Reads and updates the profile of the caller's account, and serves the avatar thumbnails that players see; guests cannot change a profile.
    /// </summary>
    [ServiceContract]
    public interface IProfileService
    {
        /// <summary>
        /// Reads the username, email and avatar reference of the caller's account.
        /// </summary>
        /// <returns>The caller's profile.</returns>
        /// <exception cref="FaultException">The profile could not be read, or the caller is a guest.</exception>
        [OperationContract(Name = "GetProfile", IsInitiating = false)]
        Task<ProfileResponse> GetProfileAsync();

        /// <summary>
        /// Starts receiving a new avatar in parts, and discards any upload the caller had left unfinished.
        /// </summary>
        /// <param name="originalBytes">Size of the image file the player chose.</param>
        /// <param name="thumbnailBytes">Size of the square thumbnail made from that image.</param>
        /// <returns>Whether the upload started, or why it was rejected.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "BeginAvatarUpload", IsInitiating = false)]
        BeginAvatarUploadResult BeginAvatarUpload(int originalBytes, int thumbnailBytes);

        /// <summary>
        /// Receives one part of the upload the caller started; parts may arrive in any order.
        /// </summary>
        /// <param name="fileKind">Whether the part belongs to the chosen image or to its thumbnail.</param>
        /// <param name="offset">Position of the part inside its file, a multiple of the part size.</param>
        /// <param name="part">Bytes of the part.</param>
        /// <returns>Whether the part was accepted, or why it was rejected.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "SendAvatarPart", IsInitiating = false)]
        SendAvatarPartResult SendAvatarPart(AvatarFileKind fileKind, int offset, byte[] part);

        /// <summary>
        /// Discards the upload the caller started, if any; nothing of it is kept.
        /// </summary>
        [OperationContract(Name = "CancelAvatarUpload", IsInitiating = false)]
        void CancelAvatarUpload();

        /// <summary>
        /// Changes the caller's username and, when asked, replaces the avatar with the one just uploaded; nothing changes if any part is rejected.
        /// </summary>
        /// <param name="username">Username the profile will have.</param>
        /// <param name="includesNewAvatar">True to use the completed upload as the new avatar; false to keep the current one.</param>
        /// <returns>Whether the profile was updated and its avatar reference, or why it was not.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "UpdateProfile", IsInitiating = false)]
        Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar);

        /// <summary>
        /// Reads the thumbnail of an avatar, as every screen shows it.
        /// </summary>
        /// <param name="avatarReference">Reference to the avatar, as profiles and player lists give it.</param>
        /// <returns>The thumbnail, or that it does not exist.</returns>
        [OperationContract(Name = "GetAvatarThumbnail", IsInitiating = false)]
        Task<AvatarResponse> GetAvatarThumbnailAsync(string avatarReference);
    }
}
