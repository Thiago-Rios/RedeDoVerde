﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedeDoVerde.Domain.Account;
using RedeDoVerde.Domain.Comment;
using RedeDoVerde.Domain.Post;
using RedeDoVerde.Repository.Context;

namespace RedeDoVerde.API.Controllers
{
    [Route("api/Comments")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly RedeDoVerdeContext _context;

        public CommentsController(RedeDoVerdeContext context)
        {
            _context = context;
        }

        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comments>>> GetComments()
        {
            return await _context.Comments.Include(x => x.Account).ToListAsync();
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentsResponse>> GetComment(Guid id)
        {
            var comment = await _context.Comments.Include(x => x.Account).Include(x => x.Post).FirstOrDefaultAsync(x => x.Id == id);

            var accountResponse = new AccountResponse { Id = comment.Account.Id, Name = comment.Account.Name };

            var postViewModel = new PostViewModel { Id = comment.Post.Id };

            var commentResponse = new CommentsResponse { Id = comment.Id, Account = accountResponse, Content = comment.Content, Post = postViewModel};

            if (comment == null)
            {
                return NotFound();
            }

            return commentResponse;
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(Guid id, Comments comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Comments
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Comments>> PostComment(Comments comment)
        {
            comment.Account = _context.Accounts.FirstOrDefault(x => x.Id == comment.Account.Id);
            comment.Post = _context.Posts.FirstOrDefault(x => x.Id == comment.Post.Id);
            comment.Id = Guid.Empty;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id }, comment);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Comments>> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        private bool CommentExists(Guid id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
