using Firebase.Auth;
using PersonDictionaryModel.FirebaseStorage.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static PersonDictionaryModel.FirebaseStorage.Constant.CredentialConstants;

namespace PersonDictionaryModel.FirebaseStorage.Service
{
    public static class StorageService
    {
        public static async Task<string> Run(Stream stream, string fullFileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(API_KEY));
            var a = await auth.SignInWithEmailAndPasswordAsync(AUTH_EMAIL, AUTH_PASSWORD);

            var cancellation = new CancellationTokenSource();

            var task = new Core.FirebaseStorage(
                BUCKET,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("assets")
                .Child(fullFileName)
                .PutAsync(stream, cancellation.Token);

            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
