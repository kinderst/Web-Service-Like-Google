/*Scott Kinder, Trie Class, PA2
 * This class is for a trie data structure. You can add to the trie to 
 * build it, or search it, too. See method comments for more details
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Trie
    {
        //Root node of trie
        public Node root { get; private set; }

        //Initilizes Tree, sets root node
        public Trie()
        {
            root = new Node { letter = Node.root };
        }

        //Adds the given word, or phrase, to the trie
        //Important, because most will be search phrases
        //pre: word cannot be null
        public void add(string fullWord)
        {
            //sets word to lower case and appends end of word char
            fullWord = fullWord.ToLower() + Node.endOfWord;

            //creates current to be able to go through and build nodes
            //up to the word
            var temp = root;

            //Adds children to each node, building up links to the word,
            //except obvious the end of word character
            foreach (var character in fullWord)
            {
                temp = temp.addChild(character);
            }
        }

        //Searches for all possible matches for words with the given substring,
        //with the given amount of desired results.
        //Returns a list of the terminal words
        public List<string> match(string substr, int numResults)
        {
            substr = substr.ToLower();

            List<string> allWords = new List<string>();

            matchRecursive(root, allWords, "", substr, numResults);
            return allWords;
        }

        //Private helper method to recursively call a match, in order to fetch
        //all the results stemming from a given substring input
        //Param node: the node being inspected, going to be branched out from
        //Param allWords: the continually building set of all terminal node words
        //Param currentStr: The current set of letters which is all the letters of the nodes in the path it took to get where it is
        //Param substring: The substring of the possible words one is searching for
        //param numResults: the max number of matches of words that one wants
        private static void matchRecursive(Node node, List<string> allWords, string currentStr, string substr, int numResults)
        {
            //if the set has already reached the desired amount
            if (allWords.Count == numResults)
            {
                return;
            }

            //if node passed is null, which happens when reached a terminal word
            if (node == null)
            {
                //if list doesn't contain the string, add the string to it, and return
                if (!allWords.Contains(currentStr))
                {
                    allWords.Add(currentStr);
                }
                return;
            }

            //concatenate the current letters to add the given node letter
            currentStr += node.letter.ToString();

            //if the string isn't empty
            if (substr.Length > 0)
            {
                //if the node has a mapping to a child with the first letter in the substring
                if (node.children.ContainsKey(substr[0]))
                {
                    //recursively call the child node, and then chop off first letter, and keep going
                    matchRecursive(node.children[substr[0]], allWords, currentStr, substr.Remove(0, 1), numResults);
                }
            }
            //else substring was empty, so get all other keys associated
            else
            {
                foreach (char key in node.children.Keys)
                {
                    matchRecursive(node.children[key], allWords, currentStr, substr, numResults);
                }
            }
        }
    }
}