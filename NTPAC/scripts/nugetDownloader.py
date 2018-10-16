#!/usr/local/bin/python3

import requests
from requests_ntlm import HttpNtlmAuth
from shutil import rmtree
from os import path
from io import BytesIO
from zipfile import ZipFile

LOCAL_PACKAGE_REPOSITORY = "/Users/vilco/Documents/VUT/marta/packages"

BASE_URL = "https://nessc.fit.vutbr.cz/tfs/Marta"
FEED_ID = "d3cd4c1e-5cd6-488c-a611-27a49aefcb8c"

USER = "nuget@nesad.fit.vutbr.cz"
PASSWORD = "59Vzo2NK6XxWUpPBNb5K"

auth = HttpNtlmAuth(USER, PASSWORD)

package_list_url = "%s/_packaging/%s/nuget/v3/query2/" % (BASE_URL, FEED_ID)
resp = requests.get(package_list_url, auth=auth)
resp.raise_for_status()
resp_data_json = resp.json()
for package in resp_data_json["data"]:
	package_id = package["id"]
	package_version = package["version"]
	package_type = package["@type"]
	print(package_id, package_version, package_type)
	if package_type != "Package":
		continue

	# Fetch package
	package_download_url = "%s/_apis/packaging/feeds/%s/nuget/packages/%s/versions/%s/content" % (BASE_URL, FEED_ID, package_id, package_version)
	package_resp = requests.get(package_download_url, auth=auth)

	# Save package to local repository
	package_file_name = "%s.%s.nupkg" % (package_id.lower(), package_version)
	package_file_path = path.join(LOCAL_PACKAGE_REPOSITORY, package_file_name)
	print("Saving to %s ..." % package_file_path)
	with open(package_file_path, "wb") as f:
		f.write(package_resp.content)
