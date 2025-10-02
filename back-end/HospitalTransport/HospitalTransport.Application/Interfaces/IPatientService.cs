using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.DTOs.Patient;

namespace HospitalTransport.Application.Interfaces
{
    public interface IPatientService
    {
        Task<BaseResponse<PatientResponse>> CreatePatientAsync(CreatePatientRequest request);
        Task<BaseResponse<PatientResponse>> UpdatePatientAsync(UpdatePatientRequest request);
        Task<BaseResponse<PatientResponse>> GetPatientByIdAsync(Guid id);
        Task<BaseResponse<IEnumerable<PatientSearchResult>>> SearchPatientsAsync(string searchTerm);
        Task<BaseResponse<bool>> DeletePatientAsync(Guid id);
    }
}
