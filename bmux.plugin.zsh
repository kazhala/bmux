0="${${ZERO:-${0:#$ZSH_ARGZERO}}:-${(%):-%N}}"
_path="${0:h}"

if [[ -z "${path[(r)${_path}]}" ]]; then
  path+=( "${_path}" )
fi

function _bmux() {
  echo "hello"
}
