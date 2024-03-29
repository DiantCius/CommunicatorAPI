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
    public class EditActivity
    {
        public class Command : IRequest<ActivityResponse>
        {
            public Command(int activityId, string action)
            {
                ActivityId = activityId;
                Action = action;
            }

            public int ActivityId { get; set; }
            public int ChildId { get; set; }
            public string Action { get; set; }
            public string Notes { get; set; }
        }
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ActivityId).NotNull().NotEmpty();
                RuleFor(x => x.ChildId).NotNull().NotEmpty();
                RuleFor(x => x.Action).NotNull().NotEmpty();
            }
        }

        public class Handler : ActivityList, IRequestHandler<Command, ActivityResponse>
        {
            private readonly ApplicationContext _context;
            private readonly CurrentUser _currentUser;

            public Handler(ApplicationContext context, CurrentUser currentUser)
            {
                _context = context;
                _currentUser = currentUser;
            }

            public async Task<ActivityResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var activityToEdit = await _context.Activities.FirstAsync(x => x.ActivityId == request.ActivityId, cancellationToken);

                if(activityToEdit == null)
                {
                    throw new ApiException($"activity with: {request.ActivityId} not found ", HttpStatusCode.NotFound);
                }

                var currentUser = await _context.Persons.FirstAsync(x => x.Username == _currentUser.GetCurrentUsername(), cancellationToken);

                if(activityToEdit.AuthorId != currentUser.PersonId)
                {
                    throw new ApiException("You are not the author", HttpStatusCode.BadRequest);
                }

                activityToEdit.Action = request.Action ?? activityToEdit.Action;
                activityToEdit.Notes = request.Notes ?? activityToEdit.Notes;

                await _context.SaveChangesAsync(cancellationToken);

                var activityList = await GetActivitiesAsync(_context, cancellationToken, request.ChildId);

                return new ActivityResponse
                {
                    Activities = activityList,
                    Count = activityList.Count(),
                };

            }

        }

    }
}
