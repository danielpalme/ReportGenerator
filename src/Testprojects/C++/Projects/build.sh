#!/bin/sh

gcc -o main -fprofile-arcs -ftest-coverage  main.cpp
./main

mkdir basic
gcov main.cpp
mv -f main.cpp.gcov basic/main.cpp.gcov
mv -f calculator.cpp.gcov basic/calculator.cpp.gcov

mkdir branch_probabilities
gcov -b main.cpp
mv -f main.cpp.gcov branch_probabilities/main.cpp.gcov
mv -f calculator.cpp.gcov branch_probabilities/calculator.cpp.gcov

mkdir branch_counts
gcov -b -c main.cpp
mv -f main.cpp.gcov branch_counts/main.cpp.gcov
mv -f calculator.cpp.gcov branch_counts/calculator.cpp.gcov

mkdir branch_unconditional
gcov -b -c -u main.cpp
mv -f main.cpp.gcov branch_unconditional/main.cpp.gcov
mv -f calculator.cpp.gcov branch_unconditional/calculator.cpp.gcov