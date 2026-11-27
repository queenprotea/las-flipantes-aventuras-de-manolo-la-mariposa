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
        /// Starts receiving a new avatar thumbnail in parts, and discards any upload the caller had left unfinished; the image the player chose never leaves the client.
        /// </summary>
        /// <param name="thumbnailBytes">Size of the square thumbnail the client made from the chosen image.</param>
        /// <returns>Whether the upload started, or why it was rejected.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "BeginAvatarUpload", IsInitiating = false)]
        BeginAvatarUploadResult BeginAvatarUpload(int thumbnailBytes);

        /// <summary>
        /// Receives one part of the thumbnail the caller started to upload; parts may arrive in any order.
        /// </summary>
        /// <param name="offset">Position of the part inside the thumbnail, a multiple of the part size.</param>
        /// <param name="part">Bytes of the part.</param>
        /// <returns>Whether the part was accepted, or why it was rejected.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "SendAvatarPart", IsInitiating = false)]
        SendAvatarPartResult SendAvatarPart(int offset, byte[] part);

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
        /// Deletes the caller's account with everything that belongs to it, takes the caller out of its room and closes its session; nothing is deleted while the caller plays a match.
        /// </summary>
        /// <returns>Whether the account was deleted, or why not.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "DeleteAccount", IsInitiating = false)]
        Task<DeleteAccountResult> DeleteAccountAsync();

        /// <summary>
        /// Reads the thumbnail of an avatar, as every screen shows it.
        /// </summary>
        /// <param name="avatarReference">Reference to the avatar, as profiles and player lists give it.</param>
        /// <returns>The thumbnail, or that it does not exist.</returns>
        [OperationContract(Name = "GetAvatarThumbnail", IsInitiating = false)]
        Task<AvatarResponse> GetAvatarThumbnailAsync(string avatarReference);
    }
}
