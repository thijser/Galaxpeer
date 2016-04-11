#!/bin/bash


if [ $# -lt 1 ]
then
	echo "Usage: $0 <number of clients>"
else
	num_clients=$1

	if [ $num_clients -gt 0 ]
	then
		echo "Starting $num_clients clients"
		for i in $(seq 1 $num_clients)
		do
			( mono --debug ./Galaxpeer-CLI.exe --connect 127.0.0.1:36963 "${@:2}" & )
		done
	fi
fi
