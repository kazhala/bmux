#!/bin/bash

mydir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

session_type="bmux"

while getopts ":a" opt; do
  case "$opt" in
    a)
      session_type="all"
      ;;
    *)
      echo "Invalid option $OPTARG" 1>&2
      exit 1
      ;;
  esac
done

sessions=()
esc=$(printf '\033')
while IFS= read -r line; do
  if grep "${line}" <(tmux list-sessions 2>/dev/null | awk -F ':' '{print $1}') &>/dev/null; then
    sessions+=("+ ${esc}[32m${line}${esc}[m")
  else
    sessions+=("- ${esc}[37m${line}${esc}[m")
  fi
done < <(find -s "${mydir}"/sessions -type f -print0 | xargs -0 -I __ basename __ .yml)

[[ "${session_type}" == 'all' ]] && \
  while IFS= read -r line; do
    [[ ! "${sessions[*]}" =~ ${line} ]] && \
      sessions+=("+ ${esc}[34m${line}${esc}[m")
  done < <(tmux list-sessions 2>/dev/null | awk -F ':' '{print $1}')

selected_sessions=$(
  IFS=$'\n'
  echo "${sessions[*]}" \
    | fzf --ansi --exit-0 --expect=ctrl-e
)

key=$(echo "${selected_sessions}" | head -1)
selected_sessions=$(echo "${selected_sessions}" | sed 1d)
[[ -z "${selected_sessions}" ]] && exit 1

# shellcheck disable=SC2046
read -r session_status session_name <<< $(echo "${selected_sessions}" | awk '{print $1 " " $2}')

if [[ -n "${key}" && "${key}" == 'ctrl-e' ]]; then
  exec nvim "${mydir}/sessions/${session_name}"
fi

[[ "${session_status}" == '-' ]] && "${mydir}"/sessions/"${session_name}"

tmux switch-client -t "${session_name}" 2>/dev/null || \
  tmux attach -t "${session_name}"
