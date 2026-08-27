using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
