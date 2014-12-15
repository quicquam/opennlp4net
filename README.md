opennlp4net is a port of OpenNLP to the .NET framework. The code is written in C#, and is very much a work-in-progress. The code in this library should not even be considered alpha, and only exists because there are no other open source natural language tools available for .NET.

The code was initially ported using an automated tool for the mechanical conversion, then attempts were made to fix functionality piece by piece, function by function, by replacing JAVA-isms with .NET equivalents. The result is not pretty, but a number of the tools work via the API:

the sentence detector

the tokenizer

the namefinder

the chunker

the postagger

the parser

Additionally the functionality relating to the SentenceDetectorME.train method also works and generates a model file compatible with OpenNLP. The website of this project is at http://www.quicquam.com

 