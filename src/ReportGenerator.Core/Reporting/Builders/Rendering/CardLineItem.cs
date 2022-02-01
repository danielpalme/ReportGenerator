using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Represents a line item within a card.
    /// </summary>
    public class CardLineItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardLineItem" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="text">The text.</param>
        /// <param name="tooltip">The tooltip.</param>
        public CardLineItem(string header, string text, string tooltip)
        {
            this.Header = header;
            this.Text = text;
            this.Tooltip = tooltip;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CardLineItem" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="text">The text.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="alignment">The alignment.</param>
        public CardLineItem(string header, string text, string tooltip, CardLineItemAlignment alignment)
        {
            this.Header = header;
            this.Text = text;
            this.Tooltip = tooltip;
            this.Alignment = alignment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CardLineItem" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="links">The links.</param>
        public CardLineItem(string header, params string[] links)
        {
            this.Header = header;
            this.Links = links;
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the tooltip.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// Gets the links.
        /// </summary>
        public IReadOnlyCollection<string> Links { get; }

        /// <summary>
        /// Gets the alignment.
        /// </summary>
        public CardLineItemAlignment Alignment { get; }
    }
}
