using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AssignmentService.Application.Interfaces.Services;

public interface IUserServiceClient
{
    Task<List<Guid>> GetUserIdsByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
}


