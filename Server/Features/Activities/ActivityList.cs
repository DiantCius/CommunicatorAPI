﻿using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Infrastructure;
using Server.Infrastructure.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Features.Activities
{
    public class ActivityList
    {
        public class Query : IRequest<ActivityResponse>
        {
            public int ChildId { get; set; }
            public Query(int id)
            {
                ChildId = id;
            }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.ChildId)
                    .NotNull()
                    .NotEmpty();
            }
        }

        public class QueryHandler : ActivityList, IRequestHandler<Query, ActivityResponse>
        {
            private readonly ApplicationContext _context;
            private readonly CurrentUser _currentUser;

            public QueryHandler(ApplicationContext context, CurrentUser currentUser)
            {
                _context = context;
                _currentUser = currentUser;
            }

            public async Task<ActivityResponse> Handle(Query request, CancellationToken cancellationToken)
            {
                var child = await _context.Children
                    .FirstAsync(x => x.ChildId == request.ChildId, cancellationToken);

                if (child == null)
                {
                    throw new ApiException("child not found", HttpStatusCode.Unauthorized);
                }

                var currentUserUsername = _currentUser.GetCurrentUsername();
                var currentUser = await _context.Persons
                    .FirstAsync(x => x.Username == currentUserUsername, cancellationToken);

                var babysitter = await _context.ChildPersons
                    .FirstOrDefaultAsync(x => x.ChildId == request.ChildId && x.PersonId == currentUser.PersonId, cancellationToken);

                if (babysitter == null)
                {
                    throw new ApiException("only babysitters can view children activities", HttpStatusCode.Unauthorized);
                }

                var activityList = await GetActivitiesAsync(_context, cancellationToken, request.ChildId);

                return new ActivityResponse
                {
                    Activities = activityList,
                    Count = activityList.Count(),
                };
            }
        }
        protected async Task<List<Activity>> GetActivitiesAsync
            (ApplicationContext applicationContext, CancellationToken cancellationToken, int childId)
        {
            var activities = await applicationContext.Activities
                .Where(x=> x.ChildId == childId)
                .OrderBy(x => x.ActivityId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var authorList = await applicationContext.Persons//.Include(x=> x.ChildPersons.Where(x=> x.ChildId == childId))
                .ToListAsync(cancellationToken);

            foreach (Activity activity in activities)
            {
                activity.Author = authorList.Find(x => x.PersonId == activity.AuthorId);
            }

            return activities;
        }

    }
}
