using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Urf
{

    public class UrfStore<TModel> : IStore<TModel> where TModel : class
    {
        private readonly IRepository<TModel> _urfRepository;
        private readonly IStoreUnitOfWork _unitOfWork;

        public UrfStore(
            IRepository<TModel> urfRepository,
            IStoreUnitOfWork unitOfWork)
        {
            _urfRepository = urfRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task SaveAsync(TModel model)
        {
            _urfRepository.Insert(model);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}