﻿using Flashcards.Core.Domain;
using Flashcards.Infrastructure.DTO;
using Flashcards.Infrastructure.Extensions;
using Flashcards.Infrastructure.IRepositories;
using Flashcards.Infrastructure.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flashcards.Infrastructure.Services
{
    public class FlashcardService : IFlashcardService
    {
        private readonly IFlashcardRepository _flashcardRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository  _categoryRepository;
        public FlashcardService(IFlashcardRepository flashcardRepository, IUserRepository userrepository,ICategoryRepository categoryRepository)
        {
            _flashcardRepository = flashcardRepository;
            _userRepository = userrepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<bool> CheckAnswer(Guid flashcardIds, string answer)
        {
            var fiskza = await _flashcardRepository.GetAsync(flashcardIds);
            if(fiskza == null)
            {
                throw new Exception("Flashcard does not exsist.");
            }
            var wynik = fiskza.CheckAndProcessAnswer(answer);
            await _flashcardRepository.UpdateAsync(fiskza);

            return wynik >= 0 ? true : false;
        }

        public async Task CreateAsync(string question, string answer, Guid categoryId, Guid flashcardId, Guid userId)
        {
            var flashcard = new Flashcard(question, answer, categoryId, flashcardId, userId);
            await _flashcardRepository.AddAsync(flashcard);
        }

        public async Task <FlashcardQuestionDto> GetAsync(Guid userId)
        {
            var flashcards = await _flashcardRepository.BrowseAsync(userId);

            var prawdziwaFiszka = flashcards.ToList().OrderBy(x => Guid.NewGuid()).First();

            var falszywe = flashcards
                .ToList()
                .Where(x => x.Id != prawdziwaFiszka.Id && x.CategoryId == prawdziwaFiszka.CategoryId && x.NextStateDate < DateTime.UtcNow)
                .OrderBy(x => Guid.NewGuid())
                .Take(3).Select(f => f.Answer);

            var answers = new List<string>();

            answers.Add(prawdziwaFiszka.Answer);
            answers.AddRange(falszywe);

            var mixedAnswers = answers.OrderBy(x => Guid.NewGuid()).ToList();

            return new FlashcardQuestionDto(prawdziwaFiszka.Id, prawdziwaFiszka.Question, mixedAnswers);
        }

        public async Task CopyFlashcardsAsync (Guid fromUserId, Guid toUserId, Guid categoryId)
        {
            var fiszki = await _flashcardRepository.BrowseAsync(fromUserId, categoryId);

            //var user = await _userRepository.GetOrFailAsync(toUserId);

            var category = await _categoryRepository.GetAsync(categoryId);

            var kopiaKategori = new Category(category.Name, toUserId);

            var kopiaFiszek = fiszki.Select(x => new Flashcard(x.Question, x.Answer, kopiaKategori.Id, Guid.NewGuid(), toUserId));

            await _categoryRepository.AddAsync(category);

            await  _flashcardRepository.AddRangeAsync(kopiaFiszek);

        }
    }
}
