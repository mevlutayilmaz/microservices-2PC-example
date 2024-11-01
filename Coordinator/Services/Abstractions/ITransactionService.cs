namespace Coordinator.Services.Abstractions
{
    public interface ITransactionService
    {
        /// <summary>
        /// Yeni bir işlem başlatır ve işlem için benzersiz bir kimlik döndürür.
        /// </summary>
        /// <returns>Oluşturulan işlemin benzersiz kimliği (GUID).</returns>
        Task<Guid> CreateTransactionAsync();

        /// <summary>
        /// İşleme dahil olan servisleri, onay süreci için hazırlar.
        /// Bu, iki-aşamalı onay (2PC) protokolünün ilk aşamasıdır.
        /// </summary>
        /// <param name="transactionId">İşlemin benzersiz kimliği.</param>
        Task PrepareServicesAsync(Guid transactionId);

        /// <summary>
        /// Tüm servislerin hazırlanıp onay için hazır olup olmadığını kontrol eder.
        /// İşleme dahil tüm servislerin 2PC protokolünün bir sonraki aşamasına geçmeye hazır olduğunu doğrular.
        /// </summary>
        /// <param name="transactionId">İşlemin benzersiz kimliği.</param>
        /// <returns>Tüm servisler hazırsa true, aksi halde false döner.</returns>
        Task<bool> CheckReadyServicesAsync(Guid transactionId);

        /// <summary>
        /// İşlemi onaylar ve tüm hazırlanan servislere değişikliklerini tamamlamalarını bildirir.
        /// Bu, 2PC protokolünün ikinci aşamasıdır.
        /// </summary>
        /// <param name="transactionId">İşlemin benzersiz kimliği.</param>
        Task CommitAsync(Guid transactionId);

        /// <summary>
        /// İşlem durumunu kontrol eder, işlem ve servislerin güncel durumlarını sorgular.
        /// Bu yöntem, işlemin başarılı bir şekilde tamamlanıp tamamlanmadığını doğrular.
        /// </summary>
        /// <param name="transactionId">İşlemin benzersiz kimliği.</param>
        /// <returns>İşlem durumu başarılıysa true, aksi halde false döner.</returns>
        Task<bool> CheckTransactionStateServicesAsync(Guid transactionId);

        /// <summary>
        /// İşlemi geri alır ve ilgili servisleri yapılan değişiklikleri iptal etmeye zorlar.
        /// İşlem sırasında bir sorun oluştuğunda çağrılır.
        /// </summary>
        /// <param name="transactionId">İşlemin benzersiz kimliği.</param>
        Task RollbackAsync(Guid transactionId);
    }

}
