variable "prefix" {
    type = string
}

variable "environment" {
    type = string
	default = "dev"
}

variable "table_name" {
    type = string
	default = ""
}

variable "topic_name" {
    type = string
	default = ""
}