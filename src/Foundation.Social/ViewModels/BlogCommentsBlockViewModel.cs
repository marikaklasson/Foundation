﻿using EPiServer.Core;
using Foundation.Social.Models.Comments;
using System.Collections.Generic;

namespace Foundation.Social.ViewModels
{
    /// <summary>
    /// The BlogCommentViewModel class represents the model that will be used to
    /// feed data to the blog comments block frontend view.
    /// </summary>
    public class BlogCommentsBlockViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">A comment form view model to get current form values for the block view model</param>
        public BlogCommentsBlockViewModel(PageReference pageReference)
        {
            CurrentPageLink = pageReference;
            Comments = new List<BlogComment>();
        }
        /// <summary>
        /// Gets or sets the reference link of the page containing the comment form.
        /// </summary>
        public PageReference CurrentPageLink { get; set; }

        /// <summary>
        /// Gets or sets the comments to show.
        /// </summary>
        public IEnumerable<BlogComment> Comments { get; set; }

        /// <summary>
        /// Gets and sets message details to be displayed to the user
        /// </summary>
        public List<string> Messages { get; set; }

        /// <summary>
        /// Gets and sets paging information
        /// </summary>
        public PagingInfo PagingInfo { get; set; }

    }
}