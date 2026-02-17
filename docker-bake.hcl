variable "GIT_BRANCH" {
  default = "develop"
}

variable "COMMIT_HASH" {
  default = "HEAD"
}

variable "BUILD_DATE" {
  default = ""
}

variable "METADATA_URL" {
  default = "https://api.bookinfo.pro"
}

variable "HARDCOVER" {
  default = "false"
}

group "default" {
  targets = ["softcover", "hardcover"]
}

target "common" {
  context = "."
  dockerfile = "docker/Dockerfile"
  platforms = ["linux/amd64", "linux/arm64"]
  args = {
    GIT_BRANCH = GIT_BRANCH
    COMMIT_HASH = COMMIT_HASH
    BUILD_DATE = BUILD_DATE
  }
}

target "softcover" {
  inherits = ["common"]
}

target "hardcover" {
  inherits = ["common"]
  args = {
    METADATA_URL = "https://hardcover.bookinfo.pro"
    HARDCOVER = "true"
  }
}
