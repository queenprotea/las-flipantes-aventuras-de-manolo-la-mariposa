using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Reads and updates the profile of the caller's account.
    /// </summary>
    [ServiceContract]
    public interface IProfileService
    {
        /// <summary>
        /// Reads the username, email and avatar of the caller's account.
        /// </summary>
        /// <returns>The caller's profile.</returns>
        [OperationContract(Name = "GetProfile")]
        Task<ProfileResponse> GetProfileAsync();

        /// <summary>
        /// Checks whether the caller can change to a username.
        /// </summary>
        /// <param name="username">Username the caller wants.</param>
        /// <returns>Whether the username is available, or why it is not.</returns>
        [OperationContract(Name = "CheckProfileUsernameAvailability")]
        Task<UsernameAvailability> CheckProfileUsernameAvailabilityAsync(string username);

        /// <summary>
        /// Changes the caller's username and, when an image is given, the avatar.
        /// </summary>
        /// <param name="username">Username the profile will have.</param>
        /// <param name="avatarImage">Image of the new avatar, or null to keep the current one.</param>
        /// <returns>Whether the profile was updated, or which change was rejected.</returns>
        [OperationContract(Name = "UpdateProfile")]
        Task<UpdateProfileResult> UpdateProfileAsync(string username, byte[]? avatarImage);

        /// <summary>
        /// Reads the image of an avatar.
        /// </summary>
        /// <param name="avatarReference">Reference to the avatar, as profiles and player lists give it.</param>
        /// <returns>The avatar image, or that it does not exist.</returns>
        [OperationContract(Name = "GetAvatar")]
        Task<AvatarResponse> GetAvatarAsync(string avatarReference);
    }
}
