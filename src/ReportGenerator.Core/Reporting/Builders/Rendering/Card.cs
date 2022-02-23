using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Represents a card in the HTML report.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Card" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public Card(string title)
        {
            this.Title = title;
            this.ProRequired = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Card" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="subtitle">The sub title.</param>
        /// <param name="subTitlePercentage">The percentage for the left border.</param>
        /// <param name="rows">The rows.</param>
        public Card(string title, string subtitle, decimal? subTitlePercentage, params CardLineItem[] rows)
        {
            this.Title = title;
            this.SubTitle = subtitle;
            this.SubTitlePercentage = subTitlePercentage;
            this.Rows = rows;
        }

        /// <summary>
        /// Gets or sets a value indicating whether PRO version is required.
        /// </summary>
        public bool ProRequired { get; set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the sub title.
        /// </summary>
        public string SubTitle { get; }

        /// <summary>
        /// Gets the percentage for the left border.
        /// </summary>
        public decimal? SubTitlePercentage { get; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        public IReadOnlyCollection<CardLineItem> Rows { get; } = new List<CardLineItem>();
    }
}
