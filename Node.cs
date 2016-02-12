using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class Node
    {

        //A dictionary mapping of all the children
        public Dictionary<char, Node> children { get; set; }

        //Defined root character
        public const char root = ' ';

        //Defined End of Word Character
        public const char endOfWord = '$';

        //The letter of the node
        public char letter { get; set; }

        //Initializes Node
        public Node() { }

        //Initializes Node, sets the letter to the given letter
        public Node(char letter)
        {
            this.letter = letter;
        }

        //Adds the given child to the node
        public Node addChild(char letter)
        {
            //Checks if given node has any children
            //if not creates dictionary for them
            if (children == null)
            {
                children = new Dictionary<char, Node>();
            }

            //Checks if the given node already has mapping for the letter
            //If not, creates a mapping to it
            if (!children.ContainsKey(letter))
            {
                //checks if letter isnt EoW letter
                if (letter == endOfWord)
                {
                    children.Add(letter, null);
                    return null;
                }
                else
                {
                    var node = new Node(letter);
                    children.Add(letter, node);
                    return node;
                }
            }

            //returns of a node representation of the child with the given letter
            return (Node)children[letter];
        }
    }
}