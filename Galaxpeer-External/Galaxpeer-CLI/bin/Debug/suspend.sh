
pid=$1
delay=$2
die=$3

if [ $die -eq 1 ]
then
    echo "Killing $pid"
    kill $pid
    sleep $delay
    echo "Starting Galaxpeer"
    mono --debug ./Galaxpeer-CLI.exe --connect 127.0.0.1:36963 --fail --measurefail &
else
    echo "Suspend $pid"
    kill -TSTP $pid
    sleep $delay
    echo "Continuing $pid"
    kill -CONT $pid
fi

