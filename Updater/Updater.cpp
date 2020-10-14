#include <iostream>
#include <string>
#include <httplib.h>
#include <nlohmann/json.hpp>
#include <filesystem>
#include <windows.h>
#include <urlmon.h>
#include <tlhelp32.h>

#pragma comment(lib, "urlmon.lib")

namespace fs = std::filesystem;

const int port = 8080;
const std::string base_adr = "ratscanner.com";
const std::string resource_path = "/api/v2/res/";
const std::string download_res = "RSDownload";
const std::string extractor_res = "RSUpdaterZipExtractor";

const std::vector<std::string> keep_files{ "Updater.exe", "config.cfg", "7za.exe" };

bool in_array(const std::string& value, const std::vector<std::string>& array)
{
	return std::find(array.begin(), array.end(), value) != array.end();
}

DWORD FindProcessId(std::string processName)
{
	PROCESSENTRY32 processInfo;
	processInfo.dwSize = sizeof(processInfo);

	HANDLE processesSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
	if (processesSnapshot == INVALID_HANDLE_VALUE)
		return 0;

	Process32First(processesSnapshot, &processInfo);
	char out1[512];
	sprintf_s(out1, "%ws", processInfo.szExeFile);
	if (!strcmp(processName.c_str(), out1))
	{
		CloseHandle(processesSnapshot);
		return processInfo.th32ProcessID;
	}

	while (Process32Next(processesSnapshot, &processInfo))
	{
		char out2[512];
		sprintf_s(out2, "%ws", processInfo.szExeFile);
		if (!strcmp(processName.c_str(), out2))
		{
			CloseHandle(processesSnapshot);
			return processInfo.th32ProcessID;
		}
	}

	CloseHandle(processesSnapshot);
	return 0;
}

void wait_for_exit(std::string process_name)
{
	while (FindProcessId(process_name)) {
		std::cout << ".";
		Sleep(100);
	}
	std::cout << std::endl;
}

void delete_old_files(fs::path executing_path)
{
	for (const auto& entry : fs::directory_iterator(executing_path))
	{
		auto file_name = entry.path().filename().string();
		if (!in_array(file_name, keep_files))
		{
			std::cout << "Removing: " + file_name << std::endl;
			fs::remove_all(file_name.c_str());
		}
	}
}

std::string get_resource(std::string resource)
{
	httplib::Client cli(base_adr, port);
	cli.set_follow_location(true);

	auto full_path = resource_path + resource;
	auto res = cli.Get(full_path.c_str());

	if (res->status != 200) return std::string();

	auto body = res->body;

	return nlohmann::json::parse(body)["value"];
}

void disable_quick_edit()
{
	HANDLE hInput;
	DWORD prev_mode;
	hInput = GetStdHandle(STD_INPUT_HANDLE);
	GetConsoleMode(hInput, &prev_mode);
	SetConsoleMode(hInput, prev_mode & ENABLE_EXTENDED_FLAGS);
}

int main(int argc, char* argv[])
{
	// Check arguments
	if (argc == 2)
	{
		std::cout << "Waiting for " + std::string(argv[1]) + " to exit";
		wait_for_exit(argv[1]);
		std::cout << "Process exited! Starting update process..." << std::endl;
		Sleep(1000);
	}
	else if (argc == 1)
	{
		std::cout << "Waiting for RatScanner.exe to exit";
		wait_for_exit("RatScanner.exe");
		std::cout << "Process exited!" << std::endl << "Starting update process..." << std::endl;
	}
	else
	{
		std::cout << "Bad arguments!" << std::endl << "First parameter has to be a valid PID" << std::endl;
		std::cout << "Press ENTER to exit." << std::endl;
		getchar();
		return ERROR_BAD_ARGUMENTS;
	}

	// Disable quick edit mode to prevent blocking
	disable_quick_edit();

	// Get executing path
	auto executing_path = ((fs::path)argv[0]).remove_filename();

	// Delete old files
	std::cout << "Removing old files..." << std::endl;
	delete_old_files(executing_path);

	// Get download path for new files
	std::cout << "Getting download url for new files.." << std::endl;
	auto download_url = get_resource(download_res);
	if (download_url.empty())
	{
		std::cout << "Error while downloading!" << std::endl;
		std::cout << "Press ENTER to exit." << std::endl;
		getchar();
		return ERROR_NETWORK_NOT_AVAILABLE;
	}

	// Download new files
	auto new_files_path = executing_path.string() + "RatScanner.zip";
	std::cout << "Downloading new files from: " + download_url << std::endl << "To: " + new_files_path << std::endl;
	URLDownloadToFileA(NULL, (LPCSTR)download_url.c_str(), (LPCSTR)new_files_path.c_str(), 0, NULL);

	// Check if zip extractor already exists
	auto extractor_path = executing_path.string() + "7za.exe";
	if (!exists(fs::path(extractor_path)))
	{
		std::cout << "Could not find zip extractor at: " + extractor_path << std::endl;

		std::cout << "Getting download url for zip extractor..." << std::endl;
		auto extractor_url = get_resource(extractor_res);
		if (extractor_url.empty())
		{
			std::cout << "Error while downloading!" << std::endl;
			std::cout << "Press ENTER to exit." << std::endl;
			getchar();
			return ERROR_NETWORK_NOT_AVAILABLE;
		}

		std::cout << "Downloading zip extractor from: " + extractor_url << std::endl << "To: " + extractor_path << std::endl;
		URLDownloadToFileA(NULL, (LPCSTR)extractor_url.c_str(), (LPCSTR)extractor_path.c_str(), 0, NULL);
	}

	// Extract new files
	std::cout << "Extracting files..." << std::endl;
	auto ret_code = system("7za.exe -aos x RatScanner.zip");

	if (ret_code != 0)
	{
		std::cout << "Something went wrong while extracting! Code: " + std::to_string(ret_code) << std::endl;
		std::cout << "Press ENTER to exit." << std::endl;
		getchar();
		return ERROR_FILE_CORRUPT;
	}

	// Delete zip archive
	std::cout << "Deleting zip archive..." << std::endl;
	fs::remove_all(new_files_path);

	std::cout << "Successfully updated!" << std::endl;

	std::cout << "Launching RatScanner.exe..." << std::endl;

	system("start RatScanner.exe");
}
