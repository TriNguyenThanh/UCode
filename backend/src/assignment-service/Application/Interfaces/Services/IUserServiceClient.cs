using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AssignmentService.Application.Interfaces.Services;

public interface IUserServiceClient
{
    Task<List<Guid>> GetStudentIdsByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
}


