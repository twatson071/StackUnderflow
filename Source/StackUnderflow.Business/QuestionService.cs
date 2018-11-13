﻿using StackUnderflow.Data;
using StackUnderflow.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using StackUnderflow.Entities.DTOs;

namespace StackUnderflow.Business
{
	public class QuestionService
	{
		private readonly ApplicationDbContext _context;

		public QuestionService(ApplicationDbContext context)
		{
			_context = context;
		}

		public IEnumerable<Question> GetQuestions()
		{
			try
			{
				return _context.Questions.ToList();
			}
			catch
			{
				throw new Exception();
			}
		}

		public QuestionDetailDto GetQuestion(int questionId)
		{
			try
			{
				var question = _context.Questions.Find(questionId);
				var answerDtos = _context.Answers.Where(a => a.QuestionId == questionId)
					.GroupJoin(_context.Comments, a => a.Id, c => c.AnswerId, (answer, comments) => new AnswerDto()
					{
						Comments = comments.ToList(),
						Answer = answer
					}).ToList();
				return new QuestionDetailDto
				{
					AnswerDtos = answerDtos,
					Question = question
				};
			}
			catch (Exception)
			{
				throw new Exception();
			}
		}

		public Question Create(Question question)
		{
			try
			{
				question.CreatedDate = DateTimeOffset.Now;
				question.AcceptedAnswerId = null;
				// question.CreatedBy = // currentUser
				_context.Questions.Add(question);
				_context.SaveChanges();
				return question;
			}
			catch
			{
				throw new Exception();
			}
		}

		public void Vote(string userId, int questionId, bool upVote)
		{
			try
			{
				var existingVote = _context.QuestionVotes.FirstOrDefault(v => v.UserId == userId && v.QuestionId == questionId);

				// if vote doesn't exist, add it
				if (existingVote == null)
				{
					_context.QuestionVotes.Add(new QuestionVote()
					{
						UserId = userId,
						QuestionId = questionId,
						Upvote = upVote
					});

				}
				// if vote exists, update it
				else
				{
					existingVote.Upvote = upVote;
				}

				_context.SaveChanges();
			}
			catch
			{
				throw new Exception();
			}
		}


		public void AcceptAnswer(int questionId, int answerId)
		{
			try
			{
				var question = _context.Questions.Find(questionId);

				if (question == null) throw new Exception("Not found");

				if (question.AcceptedAnswerId != null) throw new Exception("Already accepted an answer!");

				question.AcceptedAnswerId = answerId;

				_context.SaveChanges();
			}
			catch
			{
				throw new Exception();
			}
		}
	}
}
