\ Simple support for unit tests. In the main program, define:
\ 1 constant TESTS
\ to enable tests and wrap unit tests like
\ [ifdef] TESTS
\   ...tests...
\ [then]

[ifdef] TESTS

  -1 variable tests-OK
  : fail-tests 0 tests-OK ! ;
  : test-summary tests-OK @ if ." ** ALL OK **" else ." ** TESTS FAILED! **" then cr ;

  : =always ( n1 n2 -- ) \ assert that the two TOS values must be equal
    2dup <> if
      ." FAIL: " swap . ." <> " . fail-tests \ should print out the calling word
    else 2drop then ;

  : always ( f -- )
    0= if
      ." FAIL!" fail-tests \ should print out the calling word
    else ." OK!" then ;

[then]
