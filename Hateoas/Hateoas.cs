using System.Collections.Generic;

namespace WebAPI.Hateoas
{
    public class Hateoas
    {
        private string url { get; set; }
        private string protocol = "https://";
        public List<Link> actions = new List<Link>();

        public Hateoas(string url)
        {
            this.url = url;
        }

        public Hateoas(string url, string protocol)
        {
            this.url = url;
            this.protocol = protocol;
        }

        public void AddAction(string rel, string method)
        {
            actions.Add(new Link(this.protocol + this.url, rel, method));
        }

        public Link[] GetAction(string sufix)
        {
            Link[] tempLinks = new Link[actions.Count];

            for (int i = 0; i < tempLinks.Length; i++) {
                tempLinks[i] = new Link(actions[i].href, actions[i].rel, actions[i].method);
            }

            foreach (var link in tempLinks) {
                link.href = link.href + "/" + sufix.ToString();
            }

            return tempLinks;
        }
    }
}