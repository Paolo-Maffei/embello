\ base libraries for tve's stuff

<<<core>>>
cr compiletoflash
( tvebase start: ) here dup hex.

include ../flib/any/digits.fs

( tvebase end, size: ) here dup hex. swap - .
cornerstone <<<tvebase>>>
compiletoram
