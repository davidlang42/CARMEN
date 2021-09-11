#!/bin/bash
for i in $(seq 1992 2021);
do
	echo '{"Seed":"'$i'"}' > ./rand/random$i.rnd
	./Carmen.ExpertConverter.exe rand/random$i.rnd out/random$i.db
done
