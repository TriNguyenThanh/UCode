using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class LanguageDetailViewModel : ViewModelBase
    {
        private readonly ProblemService _problemService;
        private readonly string _problemId;
        private readonly string _languageId;
        private readonly List<ProblemLanguage> _allProblemLanguages;
        private readonly List<Language> _allLanguages;

        private string _languageDisplayName = string.Empty;
        private double _timeFactor = 1.0;
        private int _memoryKb = 262144;
        private string _head = string.Empty;
        private string _body = string.Empty;
        private string _tail = string.Empty;
        private bool _isSaving;

        public string LanguageDisplayName
        {
            get => _languageDisplayName;
            set => SetProperty(ref _languageDisplayName, value);
        }

        public double TimeFactor
        {
            get => _timeFactor;
            set => SetProperty(ref _timeFactor, value);
        }

        public int MemoryKb
        {
            get => _memoryKb;
            set => SetProperty(ref _memoryKb, value);
        }

        public string Head
        {
            get => _head;
            set => SetProperty(ref _head, value);
        }

        public string Body
        {
            get => _body;
            set => SetProperty(ref _body, value);
        }

        public string Tail
        {
            get => _tail;
            set => SetProperty(ref _tail, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler? SaveCompleted;
        public event EventHandler? CancelRequested;

        public LanguageDetailViewModel(string problemId, string languageId, ProblemService problemService, List<ProblemLanguage> allProblemLanguages, List<Language> allLanguages)
        {
            _problemId = problemId;
            _languageId = languageId;
            _problemService = problemService;
            _allProblemLanguages = allProblemLanguages;
            _allLanguages = allLanguages;

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsSaving);
            CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(this, EventArgs.Empty));
        }

        public void Initialize(ProblemLanguage problemLanguage, Language defaultLanguage)
        {
            LanguageDisplayName = problemLanguage.LanguageDisplayName;
            TimeFactor = problemLanguage.TimeFactor;
            MemoryKb = problemLanguage.MemoryKb;
            Head = problemLanguage.Head ?? defaultLanguage.DefaultHead ?? string.Empty;
            Body = problemLanguage.Body ?? defaultLanguage.DefaultBody ?? string.Empty;
            Tail = problemLanguage.Tail ?? defaultLanguage.DefaultTail ?? string.Empty;
        }

        private async Task SaveAsync()
        {
            IsSaving = true;

            try
            {
                // Build list of ALL language requests (giống web)
                var languageRequests = new List<ProblemLanguageRequest>();

                foreach (var pl in _allProblemLanguages)
                {
                    // If this is the language being edited, use new values
                    if (pl.LanguageId == _languageId)
                    {
                        languageRequests.Add(new ProblemLanguageRequest
                        {
                            ProblemId = _problemId,
                            LanguageId = _languageId,
                            IsAllowed = true,
                            TimeFactor = TimeFactor,
                            MemoryKb = MemoryKb,
                            Head = Head,
                            Body = Body,
                            Tail = Tail
                        });
                    }
                    else
                    {
                        // Keep existing values for other languages
                        var defaultLang = _allLanguages.FirstOrDefault(l => l.LanguageId == pl.LanguageId);
                        languageRequests.Add(new ProblemLanguageRequest
                        {
                            ProblemId = _problemId,
                            LanguageId = pl.LanguageId,
                            IsAllowed = true,
                            TimeFactor = pl.TimeFactor,
                            MemoryKb = pl.MemoryKb,
                            Head = pl.Head ?? defaultLang?.DefaultHead ?? string.Empty,
                            Body = pl.Body ?? defaultLang?.DefaultBody ?? string.Empty,
                            Tail = pl.Tail ?? defaultLang?.DefaultTail ?? string.Empty
                        });
                    }
                }

                var response = await _problemService.AddOrUpdateProblemLanguageAsync(_problemId, languageRequests);

                if (response?.Success == true)
                {
                    SaveCompleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    throw new Exception(response?.Message ?? "Không thể cập nhật cấu hình ngôn ngữ");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving language config: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
